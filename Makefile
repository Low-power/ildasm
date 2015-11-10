export MONO_PATH = /usr/lib/mono/2.0

MCS = gmcs
ASSEMBLIES = -r:Mono.Cecil.dll

all: 
	$(MCS) /target:exe /out:ildasm.exe $(ASSEMBLIES) @ildasm.exe.sources
