
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_GNOMEDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/libtasque.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_MDB_SOURCE=bin/Debug/libtasque.dll.mdb
LIBTASQUE_DLL_MDB=$(BUILD_DIR)/libtasque.dll.mdb

endif

if ENABLE_GNOMERELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/LinuxRelease/libtasque.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/LinuxRelease

LIBTASQUE_DLL_MDB=

endif

if ENABLE_GTKLINUXDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/libtasque.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_MDB_SOURCE=bin/Debug/libtasque.dll.mdb
LIBTASQUE_DLL_MDB=$(BUILD_DIR)/libtasque.dll.mdb

endif

if ENABLE_GTKLINUXRELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/LinuxRelease/libtasque.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/LinuxRelease

LIBTASQUE_DLL_MDB=

endif

if ENABLE_GTKWINDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/libtasque.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_MDB_SOURCE=bin/Debug/libtasque.dll.mdb
LIBTASQUE_DLL_MDB=$(BUILD_DIR)/libtasque.dll.mdb

endif

if ENABLE_GTKWINRELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/LinuxRelease/libtasque.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/LinuxRelease

LIBTASQUE_DLL_MDB=

endif

if ENABLE_GTKMACDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/libtasque.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_MDB_SOURCE=bin/Debug/libtasque.dll.mdb
LIBTASQUE_DLL_MDB=$(BUILD_DIR)/libtasque.dll.mdb

endif

if ENABLE_GTKMACRELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/LinuxRelease/libtasque.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/LinuxRelease

LIBTASQUE_DLL_MDB=

endif

if ENABLE_MONOMACDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/libtasque.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_MDB_SOURCE=bin/Debug/libtasque.dll.mdb
LIBTASQUE_DLL_MDB=$(BUILD_DIR)/libtasque.dll.mdb

endif

if ENABLE_MONOMACRELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/LinuxRelease/libtasque.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/LinuxRelease

LIBTASQUE_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(LIBTASQUE_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES = \
	IBackend.cs \
	ICategory.cs \
	INote.cs \
	ITask.cs \
	TaskPriority.cs \
	TaskState.cs \
	TaskParser.cs \
	DateFormatterFactory.cs \
	DateFormatters/IDateFormatter.cs \
	DateFormatters/WeekdayFormatter.cs \
	DateFormatters/OrdinalFormatter.cs \
	DateFormatters/WeekFormatter.cs \
	DateFormatters/StringInsensitiveComparer.cs \
	DateFormatters/QuantityFormatter.cs \
	DateFormatters/YearFormatter.cs \
	DateFormatters/DateFormatter.cs \
	DateFormatters/MonthFormatter.cs \
	DateFormatters/TodayTomorrowFormatter.cs \
	RegularExpressionFormatter.cs \
	TranslatableToken.cs \
	Extensions.cs \
	DateFormatters/DayFormatter.cs \
	DateFormatters/DateSeparatedFormatter.cs \
	INativeApplication.cs \
	Properties/AssemblyInfo.cs \
	../../build/CommonAssemblyInfo.cs \
	../../build/GlobalDefines.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	DateFormatters \
	Properties 

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	System \
	System.Core \
	Mono.Posix \
	$(GLIB_SHARP_20_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include




$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
