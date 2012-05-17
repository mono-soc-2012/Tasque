
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/tasque.tests.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../src/libtasque/bin/Release/libtasque.dll
BUILD_DIR = bin/Release

LIBTASQUE_DLL_SOURCE=../src/libtasque/bin/Release/libtasque.dll
TASQUE_TESTS_DLL_MDB=

endif

if ENABLE_LINUXDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/tasque.tests.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../src/libtasque/bin/Debug/libtasque.dll
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_SOURCE=../src/libtasque/bin/Debug/libtasque.dll
TASQUE_TESTS_DLL_MDB_SOURCE=bin/Debug/tasque.tests.dll.mdb
TASQUE_TESTS_DLL_MDB=$(BUILD_DIR)/tasque.tests.dll.mdb

endif

if ENABLE_WINDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/tasque.tests.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../src/libtasque/bin/Debug/libtasque.dll
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_SOURCE=../src/libtasque/bin/Debug/libtasque.dll
TASQUE_TESTS_DLL_MDB_SOURCE=bin/Debug/tasque.tests.dll.mdb
TASQUE_TESTS_DLL_MDB=$(BUILD_DIR)/tasque.tests.dll.mdb

endif

if ENABLE_MACDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/tasque.tests.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../src/libtasque/bin/Debug/libtasque.dll
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_SOURCE=../src/libtasque/bin/Debug/libtasque.dll
TASQUE_TESTS_DLL_MDB_SOURCE=bin/Debug/tasque.tests.dll.mdb
TASQUE_TESTS_DLL_MDB=$(BUILD_DIR)/tasque.tests.dll.mdb

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(LIBTASQUE_DLL) \
	$(TASQUE_TESTS_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES = \
	TaskParserFixture.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	System \
	$(MONO_NUNIT_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include

LIBTASQUE_DLL = $(BUILD_DIR)/libtasque.dll

$(eval $(call emit-deploy-target,LIBTASQUE_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
