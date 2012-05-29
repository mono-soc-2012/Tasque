# work around "main assembly missing" bug

inclfile="Makefile.include"

cp "$inclfile" "${inclfile}.in"

cat "${inclfile}.in" | \
sed -e 's/programfiles_DATA = $(ASSEMBLY)//' | \
sed -e 's/programfiles_DATA = $(PROGRAMFILES)/programfiles_DATA = $(ASSEMBLY) $(PROGRAMFILES)/' > "$inclfile"

rm "${inclfile}.in"
#VERSION=$(cat CommonAssemblyInfo.cs | grep "assembly: AssemblyVersion" | perl -pe "s/.*\"(.*)\".*/\1/")

#echo $VERSION

#tmp=$(tempfile)
#cat ../configure.ac | perl -pe "s/(AC_INIT\(\[tasque\], \[).+(\]\))/\${1}$VERSION\$2/" > tmp

#mv tmp ../configure.ac
