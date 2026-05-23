# image-to-md

Convert image content (screenshots, diagrams, error dialogs, tables, code) into clean Markdown using OCR.

## Compatibility

- opencode

## Description

Opens image files, runs OCR to extract visible text, converts it into clean Markdown, and saves the result alongside the original image (or in a dedicated output directory). The skill uses `markitdown` as the primary engine and falls back to `pytesseract` if the first engine is unavailable.

Designed for software engineering workflows:

- Paste a screenshot of an error dialog → get a markdown block with the message and stack trace
- Capture an Excel table → get a markdown table
- Photograph whiteboard notes → get a bullet list
- Snapshot a code snippet → get a fenced code block

## Supported Image Formats

| Format | Extension |
|--------|-----------|
| PNG    | `.png`    |
| JPEG   | `.jpg`, `.jpeg` |
| WebP   | `.webp`   |
| BMP    | `.bmp`    |

## Usage

### From the command line (direct)

```bash
python scripts/image_to_md.py screenshot.png
```

### From the command line (via npm)

```bash
npm run convert screenshot.png
```

### From OpenCode

```text
@image-to-md screenshot.png
```

## Workflow

1. User provides an image file path (or drops a screenshot into OpenCode).
2. The skill validates the file exists and has a supported extension.
3. OCR is performed (priority: markitdown → pytesseract).
4. Raw text is cleaned, structured, and formatted as Markdown.
5. A `.md` file is saved beside the original image.
6. The Markdown content is printed to stdout for OpenCode to capture.

## Output

If the input is `screenshot.png`, the output is written to `screenshot.md` in the same directory. The file is also printed to stdout so OpenCode can consume it directly.

## Example

### Input: `error-dialog.png` (screenshot of a Windows error dialog)

```
Object reference not set to an instance of an object

   at MainWindow.xaml.cs:line 55
   at App.OnStartup()
```

### Output: `error-dialog.md`

````markdown
# Error Dialog

## Message

```text
Object reference not set to an instance of an object
```

## Stack Trace

```csharp
MainWindow.xaml.cs:line 55
App.OnStartup()
```
````

## Installation

```bash
pip install -r .opencode/skills/image-to-md/requirements.txt
```

Requires Python 3.8+. On Windows, Tesseract must also be installed if using pytesseract fallback:

1. Download from https://github.com/UB-Mannheim/tesseract/wiki
2. Add `Tesseract-OCR` to your `PATH`, or set `TESSERACT_CMD` environment variable.

## Notes

- UTF-8 safe; works with Arabic, CJK, and other Unicode scripts.
- The output file is overwritten silently on repeated runs.
- Confidence scores are logged (not printed to stdout) when available from pytesseract.
