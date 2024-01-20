echo "Building PLib"

dotnet build PLib --configuration Release -p:Platform=Mergedown

# shellcheck disable=SC2181
if [ $? -ne 0 ]; then
    echo ""
    echo "!dotnet build finished with error!"
    echo ""
    echo "Press any key to exit"
    read -n 1 -s
    exit 1
fi

if [ ! -d "build" ]; then
  mkdir "build"
else
  echo "build dir is already present"
  echo "Clearing build directory"
  rm -R build/*
  echo "build directory is empty"
fi


echo "copying mod contents"
cp PLib/bin/Mergedown/Release/net471/* build/

echo "Build Done!"

echo "ILRepacking now"

IL_REPACK_VERSION='2.0.27'

IL_REPACK_PATH="$USERPROFILE\.nuget\packages\ilrepack\\$IL_REPACK_VERSION\tools\ILRepack.exe"

# For ILRepack
# WHY: to get a single dll with all required references and not spill our mod with extra files, and not to cause any conflicts with other mods 
# /out is destination file
# /lib is referenced assemblies (add game dlls here, so our mod can build)
# all other parameters are dlls that we actually merge 

"$IL_REPACK_PATH" \
  /out:build/PLib.dll \
  /lib:"C:\Users\Admin\Downloads\ONI 17.11.23\OxygenNotIncluded_Data\Managed" \
  build/PLib.dll build/PLib*.dll

# shellcheck disable=SC2181
if [ $? -ne 0 ]; then
    echo ""
    echo "!ILRepack finished with error!"
    echo ""
    echo "Press any key to exit"
    read -n 1 -s
    exit 1
fi

echo "Removing source assemblies"

find build -type f -name '*.dll' ! -name 'PLib.dll' -exec rm {} \;
find build -type f -name '*.pdb' ! -name 'PLib.pdb' -exec rm {} \;
find build -type f -name '*.xml' ! -name 'PLib.xml' -exec rm {} \;

echo ""
echo "Build SUCCEEDED!"
echo ""

echo "Press any key to exit"
read -n 1 -s