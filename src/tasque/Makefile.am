
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
BUILD_DIR = bin/Release

LIBTASQUE_DLL_SOURCE=../libtasque/bin/Release/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/Release/RtmNet.dll
TASQUE_EXE_MDB=
LIBTASQUE_DLL_MDB=
RTMNET_DLL_MDB=

endif

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/tasque.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../libtasque/bin/Debug/libtasque.dll \
	../RtmNet/bin/Debug/RtmNet.dll
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_SOURCE=../libtasque/bin/Debug/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/Debug/RtmNet.dll
TASQUE_EXE_MDB_SOURCE=bin/Debug/tasque.exe.mdb
TASQUE_EXE_MDB=$(BUILD_DIR)/tasque.exe.mdb
LIBTASQUE_DLL_MDB_SOURCE=../libtasque/bin/Debug/libtasque.dll.mdb
LIBTASQUE_DLL_MDB=$(BUILD_DIR)/libtasque.dll.mdb
RTMNET_DLL_MDB_SOURCE=../RtmNet/bin/Debug/RtmNet.dll.mdb
RTMNET_DLL_MDB=$(BUILD_DIR)/RtmNet.dll.mdb

endif

if ENABLE_LINUXDEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/tasque.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../libtasque/bin/Debug/libtasque.dll \
	../RtmNet/bin/Debug/RtmNet.dll
BUILD_DIR = bin/Debug

LIBTASQUE_DLL_SOURCE=../libtasque/bin/Debug/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/Debug/RtmNet.dll
TASQUE_EXE_MDB_SOURCE=bin/Debug/tasque.exe.mdb
TASQUE_EXE_MDB=$(BUILD_DIR)/tasque.exe.mdb
LIBTASQUE_DLL_MDB_SOURCE=../libtasque/bin/Debug/libtasque.dll.mdb
LIBTASQUE_DLL_MDB=$(BUILD_DIR)/libtasque.dll.mdb
RTMNET_DLL_MDB_SOURCE=../RtmNet/bin/Debug/RtmNet.dll.mdb
RTMNET_DLL_MDB=$(BUILD_DIR)/RtmNet.dll.mdb

endif

if ENABLE_LINUXRELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/LinuxRelease/tasque.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../libtasque/bin/LinuxRelease/libtasque.dll \
	../RtmNet/bin/LinuxRelease/RtmNet.dll
BUILD_DIR = bin/LinuxRelease

LIBTASQUE_DLL_SOURCE=../libtasque/bin/LinuxRelease/libtasque.dll
RTMNET_DLL_SOURCE=../RtmNet/bin/LinuxRelease/RtmNet.dll
TASQUE_EXE_MDB=
LIBTASQUE_DLL_MDB=
RTMNET_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(LIBTASQUE_DLL) \
	$(RTMNET_DLL) \
	$(TASQUE_EXE_MDB) \
	$(LIBTASQUE_DLL_MDB) \
	$(RTMNET_DLL_MDB)  

BINARIES = \
	$(TASQUE)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES) 

FILES = \
	RemoteControl.cs \
	RemoteControlProxy.cs \
	GnomeApplication.cs \
	Properties/AssemblyInfo.cs \
	../../build/CommonAssemblyInfo.cs \
	../../build/GlobalDefines.cs \
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

RESOURCES = \
	../../data/icons/hicolor_actions_22x22_notebook.png \
	../../data/icons/hicolor_actions_22x22_notebook.svg \
	../../data/icons/hicolor_animations_16x16_countdown-1.png \
	../../data/icons/hicolor_animations_16x16_countdown-10.png \
	../../data/icons/hicolor_animations_16x16_countdown-11.png \
	../../data/icons/hicolor_animations_16x16_countdown-12.png \
	../../data/icons/hicolor_animations_16x16_countdown-12.svg \
	../../data/icons/hicolor_animations_16x16_countdown-2.png \
	../../data/icons/hicolor_animations_16x16_countdown-3.png \
	../../data/icons/hicolor_animations_16x16_countdown-4.png \
	../../data/icons/hicolor_animations_16x16_countdown-5.png \
	../../data/icons/hicolor_animations_16x16_countdown-6.png \
	../../data/icons/hicolor_animations_16x16_countdown-7.png \
	../../data/icons/hicolor_animations_16x16_countdown-8.png \
	../../data/icons/hicolor_animations_16x16_countdown-9.png \
	../../data/icons/rtmLogo.png \
	../../data/icons/tasque.ico \
	../../data/icons/tasque.svg \
	../../data/icons/tasque-16.png \
	../../data/icons/tasque-22.png \
	../../data/icons/tasque-24.png \
	../../data/icons/tasque-256.png \
	../../data/icons/tasque-32.png \
	../../data/icons/tasque-48.png \
	../../data/icons/tasque-icons-source.svg \
	../../data/icons/tasque-opensuse-1click.png 

EXTRAS = \
	../../data/osx/Contents/Info.plist \
	../../data/osx/Contents/MacOS/Tasque \
	../../data/osx/Contents/Resources/tasque.icns \
	../../data/sounds/notify.wav \
	OSXApplication.cs \
	Properties \
	data \
	data/icons \
	data/osx \
	data/sounds \
	data/osx/Contents \
	data/osx/Contents/MacOS \
	data/osx/Contents/Resources \
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
$(eval $(call emit-deploy-target,LIBTASQUE_DLL_MDB))
$(eval $(call emit-deploy-target,RTMNET_DLL_MDB))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
