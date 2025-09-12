# hacksaw

---

**hacksaw** is a HashLink VM implementation aiming to run HashLink binaries on .NET.

## Features

- [x] Reading immutable images
- [ ] Writing immutable images
- [ ] Converting immutable images to mutable models
- [ ] Converting mutable models to immutable images
- [ ] Additional model funcitonality specialized for editing existing binaries rather than just composing new ones (game modding tools)
- [ ] Everything regarding transpiling HashLink bytecode to IL

**hacksaw** prioritizes, to some extent, memory and performance.

<img width="1808" height="221" alt="SlDJ1Ib" src="https://github.com/user-attachments/assets/77412a49-911e-4752-b382-3908711bad00" />

In idealized scenarios, it may read up to 2x faster than other libraries with half the memory. The above benchmark tests a few libraries against a small `Hello, world!` binary and the game *Dead Cells*.

These **hacksaw** scenarios permit processing only to the immutable image data with or without redundant information not typically relevant for inspection.

---

This project is adapted from and spiritually a continuation of [sharplink](https://github.com/steviegt6/sharplink), which much of the deserialization code has been reused from.

- Certain techniques and changes have been taken from the [dead-cells-core-modding fork](https://github.com/dead-cells-core-modding/core/tree/main/sources/HashlinkNET.Bytecode).
