require "rake/clean"

C_RESET = "\e[m"
C_RED = "\e[0;31m"

task :default => :build

CLEAN.include "bin/mrclc"

task :build => [
       "bin/mrclc"
     ]

file "bin/mrclc" => [
       "src/Compiler.cs",
       "src/Lexer.cs",
       "src/Parser.cs",
       "src/CodeGenerator.cs",
       "src/test/JsonTester.cs",
       "src/lib/Utils.cs",
       "src/lib/Json.cs",
       "src/lib/Types.cs",
     ] do |t|
  src_files = t.prerequisites.join(" ")

  f_out = File.join(__dir__, "z_compile_out.txt")

  sh %( mcs -debug #{src_files} -out:#{t.name} > "#{f_out}" 2>&1 ) do |ok, status|
    out = File.read(f_out)

    if ok
      print out
    else
      out.each_line do |line|
        if %r{: error CS} =~ line
          print C_RED
          print line.chomp
          print C_RESET
          print "\n"
        else
          puts line
        end
      end
      exit status.exitstatus
    end
  end

end
