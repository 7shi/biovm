# BioVM

Artificial life using the Brainfuck-like language, which mimics Tierra.

![screen shot](https://github.com/7shi/biovm/blob/main/img/screenshot.png?raw=true)

## Instruction Set

Extended Brainfuck by adding pointer registers `r1` and `r2`.

Defined in Form1.cs:

```csharp
private string[] mnemonic = new[]
{
    /* 0 */ "halt",
    /* 1 */ "inc r0",
    /* 2 */ "inc r1",
    /* 3 */ "inc r2",
    /* 4 */ "dec r0",
    /* 5 */ "dec r1",
    /* 6 */ "dec r2",
    /* 7 */ "mov r0, [r1]",
    /* 8 */ "mov r0, [r2]",
    /* 9 */ "mov [r1], r0",
    /* a */ "mov [r2], r0",
    /* b */ "while [r1]",
    /* c */ "while true",
    /* d */ "wend",
    /* e */ "rand r2",
    /* f */ "join r2",
};
```

## Code Template

Self-replicating code that starts the mutation. r1 points to the address of the self, and copies it to the random address r2. `join r2` gives it life.

Defined in Form1.cs:

```csharp
private byte[] Template = new byte[]
{
    0xc, // while true
    0xb, //   while [r1]
    0x7, //     mov r0, [r1]
    0x2, //     inc r1
    0xa, //     mov [r2], r0
    0x3, //     inc r2
    0xd, //   wend
    0x5, //   dec r1
    0xb, //   while [r1]
    0x5, //     dec r1
    0x6, //     dec r2
    0xd, //   wend
    0x2, //   inc r1
    0xf, //   join r2
    0xe, //   rand r2
    0xd, // wend
    0x0, // halt
};
```
