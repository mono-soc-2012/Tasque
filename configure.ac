dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([tasque], [0.1.10])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROG(DMCS, dmcs, no)
if test "x$DMCS" = "xno"; then
        AC_MSG_ERROR([dmcs Not found])
fi


AC_ARG_ENABLE(gnomedebug,
	AC_HELP_STRING([--enable-gnomedebug],
		[Use 'GNOMEDEBUG' Configuration [default=NO]]),
		enable_gnomedebug=yes, enable_gnomedebug=no)
AM_CONDITIONAL(ENABLE_GNOMEDEBUG, test x$enable_gnomedebug = xyes)
if test "x$enable_gnomedebug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(gnomerelease,
	AC_HELP_STRING([--enable-gnomerelease],
		[Use 'GNOMERELEASE' Configuration [default=YES]]),
		enable_gnomerelease=yes, enable_gnomerelease=no)
AM_CONDITIONAL(ENABLE_GNOMERELEASE, test x$enable_gnomerelease = xyes)
if test "x$enable_gnomerelease" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(gtklinuxdebug,
	AC_HELP_STRING([--enable-gtklinuxdebug],
		[Use 'GTKLINUXDEBUG' Configuration [default=NO]]),
		enable_gtklinuxdebug=yes, enable_gtklinuxdebug=no)
AM_CONDITIONAL(ENABLE_GTKLINUXDEBUG, test x$enable_gtklinuxdebug = xyes)
if test "x$enable_gtklinuxdebug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(gtklinuxrelease,
	AC_HELP_STRING([--enable-gtklinuxrelease],
		[Use 'GTKLINUXRELEASE' Configuration [default=NO]]),
		enable_gtklinuxrelease=yes, enable_gtklinuxrelease=no)
AM_CONDITIONAL(ENABLE_GTKLINUXRELEASE, test x$enable_gtklinuxrelease = xyes)
if test "x$enable_gtklinuxrelease" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(gtkwindebug,
	AC_HELP_STRING([--enable-gtkwindebug],
		[Use 'GTKWINDEBUG' Configuration [default=NO]]),
		enable_gtkwindebug=yes, enable_gtkwindebug=no)
AM_CONDITIONAL(ENABLE_GTKWINDEBUG, test x$enable_gtkwindebug = xyes)
if test "x$enable_gtkwindebug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(gtkwinrelease,
	AC_HELP_STRING([--enable-gtkwinrelease],
		[Use 'GTKWINRELEASE' Configuration [default=NO]]),
		enable_gtkwinrelease=yes, enable_gtkwinrelease=no)
AM_CONDITIONAL(ENABLE_GTKWINRELEASE, test x$enable_gtkwinrelease = xyes)
if test "x$enable_gtkwinrelease" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(gtkmacdebug,
	AC_HELP_STRING([--enable-gtkmacdebug],
		[Use 'GTKMACDEBUG' Configuration [default=NO]]),
		enable_gtkmacdebug=yes, enable_gtkmacdebug=no)
AM_CONDITIONAL(ENABLE_GTKMACDEBUG, test x$enable_gtkmacdebug = xyes)
if test "x$enable_gtkmacdebug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(gtkmacrelease,
	AC_HELP_STRING([--enable-gtkmacrelease],
		[Use 'GTKMACRELEASE' Configuration [default=NO]]),
		enable_gtkmacrelease=yes, enable_gtkmacrelease=no)
AM_CONDITIONAL(ENABLE_GTKMACRELEASE, test x$enable_gtkmacrelease = xyes)
if test "x$enable_gtkmacrelease" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(monomacdebug,
	AC_HELP_STRING([--enable-monomacdebug],
		[Use 'MONOMACDEBUG' Configuration [default=NO]]),
		enable_monomacdebug=yes, enable_monomacdebug=no)
AM_CONDITIONAL(ENABLE_MONOMACDEBUG, test x$enable_monomacdebug = xyes)
if test "x$enable_monomacdebug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(monomacrelease,
	AC_HELP_STRING([--enable-monomacrelease],
		[Use 'MONOMACRELEASE' Configuration [default=NO]]),
		enable_monomacrelease=yes, enable_monomacrelease=no)
AM_CONDITIONAL(ENABLE_MONOMACRELEASE, test x$enable_monomacrelease = xyes)
if test "x$enable_monomacrelease" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_GNOMERELEASE, true)
	enable_gnomerelease=yes
fi


dnl package checks, common for all configs
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
PKG_CHECK_MODULES([GLIB_SHARP_20], [glib-sharp-2.0])
PKG_CHECK_MODULES([MONO_NUNIT], [mono-nunit])
PKG_CHECK_MODULES([NDESK_DBUS_10], [ndesk-dbus-1.0])
PKG_CHECK_MODULES([GNOME_SHARP_20], [gnome-sharp-2.0])
PKG_CHECK_MODULES([NOTIFY_SHARP], [notify-sharp])
PKG_CHECK_MODULES([NDESK_DBUS_GLIB_10], [ndesk-dbus-glib-1.0])

dnl package checks, per config


AC_CONFIG_FILES([
src/libtasque/Makefile
src/RtmNet/Makefile
tests/Makefile
src/tasque/tasque
src/tasque/Makefile
Makefile

])

AC_OUTPUT
