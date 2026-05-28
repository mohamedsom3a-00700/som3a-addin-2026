# Duration Estimator Plugin — Contracts

**Phase**: 1 — Design & Contracts
**Date**: 2026-05-28

This directory defines the interface contracts for the Duration Estimator Plugin. Contracts are organized by consumer:

| Contract | Consumer | Description |
|----------|----------|-------------|
| `plugin-contract.md` | Som3a.Plugin.SDK PluginHost | How the plugin registers and exposes its capabilities |
| `engine-contract.md` | Internal modules | Interfaces between plugin sub-modules (ProductivityEngine, Calendar, etc.) |
| `scheduling-pipeline-contract.md` | Downstream scheduling tools | Data format for feeding duration estimates into Primavera-compatible scheduling |
