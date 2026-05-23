"""
image_to_md.py — OCR image to Markdown conversion.

Usage:
    python image_to_md.py <image_path>

Priority:
    1. markitdown  (preferred, handles structured content)
    2. pytesseract (fallback, raw OCR with layout analysis)

Output:
    - Saves <image_name>.md beside the original image.
    - Prints Markdown to stdout for OpenCode consumption.
"""

from __future__ import annotations

import argparse
import logging
import os
import re
import sys
from pathlib import Path

# ---------------------------------------------------------------------------
# Logging — quiet by default, visible on stderr if --verbose is passed
# ---------------------------------------------------------------------------

logging.basicConfig(
    level=logging.INFO,
    format="[image-to-md] %(levelname)s %(message)s",
    stream=sys.stderr,
)
log = logging.getLogger("image_to_md")

# ---------------------------------------------------------------------------
# Supported image extensions
# ---------------------------------------------------------------------------

SUPPORTED_EXTENSIONS: set[str] = {".png", ".jpg", ".jpeg", ".webp", ".bmp"}

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _output_path(image_path: Path, output_dir: Path | None) -> Path:
    """Derive the .md output path."""
    if output_dir:
        output_dir.mkdir(parents=True, exist_ok=True)
        return output_dir / f"{image_path.stem}.md"
    return image_path.with_suffix(".md")


def _clean_markdown(text: str) -> str:
    """
    Post-process raw OCR output into cleaner Markdown.

    - Collapse 3+ consecutive blank lines into 2.
    - Remove trailing whitespace on each line.
    - Ensure the file ends with exactly one newline.
    - Wrap isolated code-looking lines in fenced blocks.
    """
    lines = text.splitlines()
    cleaned: list[str] = []
    blank_count = 0

    for line in lines:
        stripped = line.rstrip()
        if stripped == "":
            blank_count += 1
            if blank_count > 2:
                continue
            cleaned.append("")
        else:
            blank_count = 0
            cleaned.append(stripped)

    raw = "\n".join(cleaned)
    raw = re.sub(r" {2,}", " ", raw)

    if not raw.endswith("\n"):
        raw += "\n"

    return raw


def _auto_fence_code(text: str) -> str:
    """
    Detect lines that look like code and wrap them in fenced blocks.

    Heuristic: if 3+ consecutive lines are indented or contain
    operator-heavy content, treat them as a code block.
    """
    lines = text.splitlines()
    result: list[str] = []
    buffer: list[str] = []
    in_code = False

    code_pattern = re.compile(
        r"^(\s{2,}|[a-zA-Z]:\\)|[{}();=<>+*/&|!~^%@#$]"
    )

    for line in lines:
        is_code_like = bool(code_pattern.search(line)) and len(line.strip()) > 0

        if is_code_like:
            buffer.append(line)
        else:
            if len(buffer) >= 3:
                result.append("```text")
                result.extend(buffer)
                result.append("```")
                in_code = True
            elif buffer:
                result.extend(buffer)
            buffer = []
            result.append(line)

    # flush remaining buffer
    if len(buffer) >= 3:
        result.append("```text")
        result.extend(buffer)
        result.append("```")
    elif buffer:
        result.extend(buffer)

    return "\n".join(result)


def _try_language_tag(text: str) -> str:
    """
    Try to tag fenced code blocks with a language hint.

    Very basic heuristic — looks at extension-like patterns or known
    keywords inside the first line of the block.
    """
    lang_map = {
        "csharp": "csharp",
        "c#": "csharp",
        "xaml": "xml",
        "xml": "xml",
        "python": "python",
        "py": "python",
        "javascript": "javascript",
        "js": "javascript",
        "typescript": "typescript",
        "ts": "typescript",
        "json": "json",
        "sql": "sql",
        "ps": "powershell",
        "powershell": "powershell",
        "bash": "bash",
        "shell": "bash",
        "cmd": "dos",
        "dos": "dos",
        "text": "text",
    }

    def _replace_lang(match: re.Match) -> str:
        body = match.group(2)
        first_line = body.strip().split("\n")[0].lower()
        for keyword, lang in lang_map.items():
            if keyword in first_line:
                return f"```{lang}\n{body}\n```"
        return f"```text\n{body}\n```"

    return re.sub(
        r"```text\n(.*?)\n```",
        _replace_lang,
        text,
        flags=re.DOTALL,
    )


# ---------------------------------------------------------------------------
# OCR Engines
# ---------------------------------------------------------------------------


def _ocr_markitdown(image_path: Path) -> str | None:
    """
    Attempt OCR via MarkItDown.

    Returns the extracted text as a string, or None if unavailable / fails.
    """
    try:
        from markitdown import MarkItDown

        md = MarkItDown()
        result = md.convert(str(image_path))
        text = result.text_content if hasattr(result, "text_content") else str(result)
        log.info("markitdown succeeded — extracted %d characters", len(text))
        return text
    except Exception as exc:
        log.warning("markitdown failed: %s", exc)
        return None


def _ocr_pytesseract(image_path: Path) -> tuple[str, float | None]:
    """
    Fallback OCR via pytesseract with Arabic + English support.

    Returns (text, confidence_or_None).
    """
    try:
        from PIL import Image
        import pytesseract

        img = Image.open(image_path)

        # Attempt Arabic + English; fall back to English-only if ara data missing
        try:
            data = pytesseract.image_to_data(
                img, lang="ara+eng", output_type=pytesseract.Output.DICT
            )
        except Exception:
            log.info("Arabic OCR language data not available; falling back to English")
            data = pytesseract.image_to_data(
                img, lang="eng", output_type=pytesseract.Output.DICT
            )

        # Build text from data dict while computing average confidence
        lines: list[str] = []
        confs: list[float] = []
        current_line = ""

        for i in range(len(data["text"])):
            text = data["text"][i].strip()
            conf = int(data["conf"][i]) if data["conf"][i] != "-1" else 0

            if data["line_num"][i] != (i > 0 and data["line_num"][i - 1]):
                if current_line:
                    lines.append(current_line)
                    current_line = ""
                current_line = text
            else:
                if current_line:
                    current_line += " " + text
                else:
                    current_line = text
            confs.append(float(conf))

        if current_line:
            lines.append(current_line)

        avg_conf = sum(confs) / len(confs) if confs else None
        log.info(
            "pytesseract succeeded — %d chars, avg confidence %.1f%%",
            sum(len(l) for l in lines),
            avg_conf or 0,
        )

        return "\n".join(lines), avg_conf

    except Exception as exc:
        log.error("pytesseract failed: %s", exc)
        return "", None


# ---------------------------------------------------------------------------
# Core conversion
# ---------------------------------------------------------------------------


def convert_image(image_path: Path, output_dir: Path | None = None) -> str | None:
    """
    Convert an image file to Markdown.

    Parameters
    ----------
    image_path : Path
        Path to the image file.
    output_dir : Path or None
        Optional output directory. If None, .md is written beside the image.

    Returns
    -------
    str or None
        The extracted Markdown text, or None on failure.
    """
    # Validate file
    if not image_path.exists():
        log.error("File not found: %s", image_path)
        return None

    ext = image_path.suffix.lower()
    if ext not in SUPPORTED_EXTENSIONS:
        log.error(
            "Unsupported extension '%s'. Supported: %s",
            ext,
            ", ".join(sorted(SUPPORTED_EXTENSIONS)),
        )
        return None

    log.info("Processing: %s", image_path)

    # --- OCR ---
    text = _ocr_markitdown(image_path)

    confidence: float | None = None
    if text is None:
        log.info("Falling back to pytesseract...")
        text, confidence = _ocr_pytesseract(image_path)

    if not text or not text.strip():
        log.warning("No text extracted from image.")
        return None

    # --- Post-processing ---
    markdown = _clean_markdown(text)
    markdown = _auto_fence_code(markdown)
    markdown = _try_language_tag(markdown)

    # --- Write output file ---
    out_path = _output_path(image_path, output_dir)
    try:
        out_path.write_text(markdown, encoding="utf-8")
        log.info("Saved: %s", out_path)
    except Exception as exc:
        log.error("Failed to write output: %s", exc)
        return None

    if confidence is not None:
        log.info("OCR confidence: %.1f%%", confidence)

    return markdown


# ---------------------------------------------------------------------------
# CLI entry point
# ---------------------------------------------------------------------------


def _parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Convert image content to Markdown via OCR.",
    )
    parser.add_argument("image", type=str, help="Path to the image file")
    parser.add_argument(
        "--output-dir",
        "-o",
        type=str,
        default=None,
        help="Output directory for .md file (default: same directory as image)",
    )
    parser.add_argument(
        "--verbose",
        "-v",
        action="store_true",
        help="Show verbose logging on stderr",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = _parse_args(argv)

    if args.verbose:
        logging.getLogger("image_to_md").setLevel(logging.DEBUG)

    image_path = Path(args.image).resolve()
    output_dir = Path(args.output_dir).resolve() if args.output_dir else None

    markdown = convert_image(image_path, output_dir)

    if markdown is None:
        log.error("Conversion failed.")
        return 1

    # Print Markdown to stdout for OpenCode consumption
    print(markdown, end="")
    return 0


if __name__ == "__main__":
    sys.exit(main())
