This is a port of the compiler part of [vm2gol-v2 (Ruby version)](https://github.com/sonota88/vm2gol-v2).

```
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
  389 src/CodeGenerator.cs
   28 src/Compiler.cs
   70 src/Lexer.cs
  402 src/Parser.cs
  221 src/lib/Types.cs
   78 src/lib/Utils.cs
 1188 total

  $ cat src/*.cs src/lib/{Types,Utils}.cs | grep -v '^ *//' | wc -l
1182

  $ wc -l src/lib/Json.cs
101 src/lib/Json.cs
```
