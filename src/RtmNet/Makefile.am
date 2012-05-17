
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/RtmNet.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release/

RTMNET_DLL_MDB=

endif

if ENABLE_LINUXDEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/RtmNet.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug/

RTMNET_DLL_MDB_SOURCE=bin/Debug/RtmNet.dll.mdb
RTMNET_DLL_MDB=$(BUILD_DIR)/RtmNet.dll.mdb

endif

if ENABLE_WINDEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/RtmNet.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug/

RTMNET_DLL_MDB_SOURCE=bin/Debug/RtmNet.dll.mdb
RTMNET_DLL_MDB=$(BUILD_DIR)/RtmNet.dll.mdb

endif

if ENABLE_MACDEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/RtmNet.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug/

RTMNET_DLL_MDB_SOURCE=bin/Debug/RtmNet.dll.mdb
RTMNET_DLL_MDB=$(BUILD_DIR)/RtmNet.dll.mdb

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

LINUX_PKGCONFIG = \
	$(RTMNET_PC)  

PROGRAMFILES = \
	$(RTMNET_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(LINUX_PKGCONFIG) $(PROGRAMFILES) 

FILES = \
	ApiKeyRequiredException.cs \
	Auth.cs \
	AuthenticationRequiredException.cs \
	Categories.cs \
	Contacts.cs \
	DateGranularity.cs \
	Enums.cs \
	Groups.cs \
	GroupSearchResults.cs \
	List.cs \
	Methods.cs \
	Note.cs \
	Response.cs \
	ResponseXmlException.cs \
	Rtm.cs \
	RtmApiException.cs \
	RtmException.cs \
	RtmWebException.cs \
	SignatureRequiredException.cs \
	Tags.cs \
	Task.cs \
	User.cs \
	Utils.cs \
	Properties/AssemblyInfo.cs \
	../../build/CommonAssemblyInfo.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	example_app.config \
	License.txt \
	Properties \
	rtmnet.pc.in 

REFERENCES =  \
	System \
	System.Web \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(LINUX_PKGCONFIG) $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include

RTMNET_PC = $(BUILD_DIR)/rtmnet.pc

$(eval $(call emit-deploy-wrapper,RTMNET_PC,rtmnet.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
