This is a port of the compiler part of [vm2gol-v2 (Ruby version)](https://github.com/sonota88/vm2gol-v2).

C#で簡単な自作言語のコンパイラを書いた  
https://qiita.com/sonota88/items/c0a5bf76d7eb3aa4c507


```
  $ dotnet --version
8.0.120
```

```
git clone --recursive https://github.com/sonota88/mini-ruccola-csharp.git
cd mini-ruccola-csharp

./docker.sh build
./test.sh all
```

```
  $ LANG=C wc -l src/{Lexer,Parser,CodeGenerator}.cs
   73 src/Lexer.cs
  430 src/Parser.cs
  422 src/CodeGenerator.cs
  925 total
```
