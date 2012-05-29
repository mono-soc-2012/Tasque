# work around "main assembly missing" bug
inclfile="Makefile.include"
cp "$inclfile" "${inclfile}.in"
cat "${inclfile}.in" | \
sed -e 's/programfiles_DATA = $(ASSEMBLY)//' | \
sed -e 's/programfiles_DATA = $(PROGRAMFILES)/programfiles_DATA = $(ASSEMBLY) $(PROGRAMFILES)/' > "$inclfile"
rm "${inclfile}.in"


# fix bin script: add "-a Tasque" so that proper process name is used
binfile="src/tasque/tasque.in"
cp "$binfile" "${binfile}.in"
cat "${binfile}.in" | \
sed -e 's/#!\/bin\/sh/#!\/bin\/bash/' | \
sed -e 's/exec mono "@expanded_libdir@\/@PACKAGE@\/tasque.exe" "$@"/exec -a "Tasque" mono "@expanded_libdir@\/@PACKAGE@\/tasque.exe" "$@"/' > "$binfile"
rm "${binfile}.in"
