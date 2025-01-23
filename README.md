# NetSDR Client

A .NET client for interfacing with NetSDR devices via TCP/IP protocol.

## Architecture

- NetSdrClient - Main client interface
- NetworkClient - TCP communication handler
- IqDataProcessor - UDP data receiver
- Protocol namespace - Message parsing and protocol implementation

## Performance

- Minimal memory allocations
- Efficient UDP data processing
- Thread-safe operation
- Benchmark results available in docs/Benchmark/BenchmarkResults.png