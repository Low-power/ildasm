ifndef MONO_PATH
export MONO_PATH = /usr/lib/mono/2.0
endif

MCS := gmcs

ASSEMBLIES := -r:Mono.Cecil.dll

ildasm.exe:	ildasm.exe.sources
	$(MCS) @ildasm.exe.sources -out:$@ $(ASSEMBLIES)
