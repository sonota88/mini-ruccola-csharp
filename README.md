This is a port of the compiler part of [vm2gol-v2 (Ruby version)](https://github.com/sonota88/vm2gol-v2).

C#で簡単な自作言語のコンパイラを書いた  
https://qiita.com/sonota88/items/c0a5bf76d7eb3aa4c507


```
  $ apt show mono-devel 2>/dev/null | grep Version
Version: 6.8.0.105+dfsg-3.6ubuntu2

  $ mcs -help | grep VERSION
   -sdk:VERSION         Specifies SDK version of referenced assemblies
                        VERSION can be one of: 2, 4, 4.5 (default) or a custom value
```

```
git clone --recursive https://github.com/sonota88/mini-ruccola-csharp.git
cd mini-ruccola-csharp

./docker.sh build
./test.sh all
```

```
  $ LANG=C wc -l src/*.cs src/lib/{Types,Utils}.cs
  422 src/CodeGenerator.cs
   28 src/Compiler.cs
   73 src/Lexer.cs
  430 src/Parser.cs
  242 src/lib/Types.cs
   66 src/lib/Utils.cs
 1261 total

  $ cat src/*.cs src/lib/{Types,Utils}.cs | grep -v '^ *//' | wc -l
1256

  $ wc -l src/lib/Json.cs
106 src/lib/Json.cs
```
