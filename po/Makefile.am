
EXTRA_DIST =  


# Warning: This is an automatically generated file, do not edit!

CLEANFILES = $(GMO_FILES)

include $(top_srcdir)/Makefile.include

PACKAGE = @PACKAGE@
BUILD_DIR = .
INSTALL_DIR = $(DESTDIR)@expanded_libdir@/$(PACKAGE)/locale

FILES = \
	ca.po\
	ca@valencia.po\
	cs.po\
	da.po\
	de.po\
	el.po\
	en_GB.po\
	eo.po\
	es.po\
	et.po\
	fi.po\
	fr.po\
	gl.po\
	id.po\
	it.po\
	ja.po\
	nb.po\
	nds.po\
	nl.po\
	ru.po\
	pl.po\
	pt.po\
	pt_BR.po\
	ro.po\
	sl.po\
	sr.po\
	sr@latin.po\
	sv.po\
	th.po\
	tr.po\
	zh_CN.po\
	zh_TW.po

GMO_FILES = $(patsubst %.po,$(BUILD_DIR)/%.gmo,$(FILES))
MO_FILES = $(foreach po,$(FILES), $(INSTALL_DIR)/$(basename $(po))/LC_MESSAGES/$(PACKAGE).mo)

all: $(GMO_FILES)

install: $(MO_FILES)

uninstall:
	for lang in $(basename $(FILES)); do \
		rm -f $(INSTALL_DIR)/$$lang/LC_MESSAGES/$(PACKAGE).mo; \
	done

$(GMO_FILES): $(BUILD_DIR)/%.gmo: %.po
	mkdir -p $(BUILD_DIR)
	msgfmt '$<' -o '$@'
	
$(MO_FILES): $(INSTALL_DIR)/%/LC_MESSAGES/$(PACKAGE).mo : $(BUILD_DIR)/%.gmo
	mkdir -p $(dir $@)
	@INSTALL_DATA@ '$<' '$@'