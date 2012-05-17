
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/tasque.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../libtasque/bin/Release/libtasque.dll \
	../RtmNet/bin/Release/RtmNet.dll
BUILD_DIR = bin/Release/

LIBTASQUE_DLL_SOURCE=../libtasque/bin/Release/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/Release/RtmNet.dll
TASQUE_EXE_MDB=

endif

if ENABLE_LINUXDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;ENABLE_NOTIFY_SHARP"
ASSEMBLY = bin/Debug/tasque.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../libtasque/bin/Debug/libtasque.dll \
	../RtmNet/bin/Debug/RtmNet.dll
BUILD_DIR = bin/Debug/

LIBTASQUE_DLL_SOURCE=../libtasque/bin/Debug/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/Debug/RtmNet.dll
TASQUE_EXE_MDB_SOURCE=bin/Debug/tasque.exe.mdb
TASQUE_EXE_MDB=$(BUILD_DIR)/tasque.exe.mdb

endif

if ENABLE_WINDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;WIN32"
ASSEMBLY = bin/Debug/tasque.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../libtasque/bin/Debug/libtasque.dll \
	../RtmNet/bin/Debug/RtmNet.dll
BUILD_DIR = bin/Debug/

LIBTASQUE_DLL_SOURCE=../libtasque/bin/Debug/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/Debug/RtmNet.dll
TASQUE_EXE_MDB_SOURCE=bin/Debug/tasque.exe.mdb
TASQUE_EXE_MDB=$(BUILD_DIR)/tasque.exe.mdb

endif

if ENABLE_MACDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;OSX"
ASSEMBLY = bin/Debug/tasque.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../libtasque/bin/Debug/libtasque.dll \
	../RtmNet/bin/Debug/RtmNet.dll
BUILD_DIR = bin/Debug/

LIBTASQUE_DLL_SOURCE=../libtasque/bin/Debug/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/Debug/RtmNet.dll
TASQUE_EXE_MDB_SOURCE=bin/Debug/tasque.exe.mdb
TASQUE_EXE_MDB=$(BUILD_DIR)/tasque.exe.mdb

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(LIBTASQUE_DLL) \
	$(RTMNET_DLL) \
	$(TASQUE_EXE_MDB)  

BINARIES = \
	$(TASQUE)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES) 

FILES = \
	Defines.WIN32.cs \
	RemoteControl.cs \
	RemoteControlProxy.cs \
	GnomeApplication.cs \
	Properties/AssemblyInfo.cs \
	../../build/CommonAssemblyInfo.cs \
	AbstractTask.cs \
	AllCategory.cs \
	Application.cs \
	Backends/Dummy/DummyBackend.cs \
	Backends/Dummy/DummyCategory.cs \
	Backends/Dummy/DummyNote.cs \
	Backends/Dummy/DummyTask.cs \
	Backends/Hiveminder/HmBackend.cs \
	Backends/Hiveminder/HmCategory.cs \
	Backends/Hiveminder/HmNote.cs \
	Backends/Hiveminder/HmPreferencesWidget.cs \
	Backends/Hiveminder/HmTask.cs \
	Backends/Hiveminder/service/Group.cs \
	Backends/Hiveminder/service/Hiveminder.cs \
	Backends/Hiveminder/service/HiveminderException.cs \
	Backends/Hiveminder/service/Task.cs \
	Backends/Rtm/RtmBackend.cs \
	Backends/Rtm/RtmCategory.cs \
	Backends/Rtm/RtmNote.cs \
	Backends/Rtm/RtmPreferencesWidget.cs \
	Backends/Rtm/RtmTask.cs \
	Backends/Sqlite/Database.cs \
	Backends/Sqlite/SqliteBackend.cs \
	Backends/Sqlite/SqliteCategory.cs \
	Backends/Sqlite/SqliteNote.cs \
	Backends/Sqlite/SqliteTask.cs \
	CellRendererDate.cs \
	CompletedTaskGroup.cs \
	CompletedTaskGroupModel.cs \
	DateButton.cs \
	GtkApplication.cs \
	Logger.cs \
	NoteDialog.cs \
	NoteWidget.cs \
	Preferences.cs \
	PreferencesDialog.cs \
	TaskCalendar.cs \
	TaskGroup.cs \
	TaskGroupModel.cs \
	TaskGroupModelFactory.cs \
	TaskTreeView.cs \
	TaskWindow.cs \
	Utilities.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	res/tasque-opensuse-1click.png \
	res/images/clock-16-0.png \
	res/images/clock-16-0.svg \
	res/images/clock-16-1.png \
	res/images/clock-16-10.png \
	res/images/clock-16-11.png \
	res/images/clock-16-2.png \
	res/images/clock-16-3.png \
	res/images/clock-16-4.png \
	res/images/clock-16-5.png \
	res/images/clock-16-6.png \
	res/images/clock-16-7.png \
	res/images/clock-16-8.png \
	res/images/clock-16-9.png \
	res/images/note-16.png \
	res/images/note-22.svg \
	res/images/rtmLogo.png \
	res/images/tasque-16.png \
	res/images/tasque-22.png \
	res/images/tasque-24.png \
	res/images/tasque-256.png \
	res/images/tasque-32.png \
	res/images/tasque-48.png \
	res/images/tasque-icons-source.svg \
	res/images/tasque.ico \
	res/images/tasque.svg \
	res/osx/Contents/Info.plist \
	res/osx/Contents/MacOS/Tasque \
	res/osx/Contents/Resources/tasque.icns \
	res/sounds/notify.wav \
	OSXApplication.cs \
	Properties \
	tasque.in 

REFERENCES =  \
	System \
	System.Data \
	System.Xml \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	$(NDESK_DBUS_10_LIBS) \
	Mono.Posix \
	Mono.Data.Sqlite \
	$(GNOME_SHARP_20_LIBS) \
	$(NOTIFY_SHARP_LIBS) \
	$(NDESK_DBUS_GLIB_10_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

include $(top_srcdir)/Makefile.include

LIBTASQUE_DLL = $(BUILD_DIR)/libtasque.dll
RTMNET_DLL = $(BUILD_DIR)/RtmNet.dll
TASQUE = $(BUILD_DIR)/tasque

$(eval $(call emit-deploy-target,LIBTASQUE_DLL))
$(eval $(call emit-deploy-target,RTMNET_DLL))
$(eval $(call emit-deploy-wrapper,TASQUE,tasque,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
