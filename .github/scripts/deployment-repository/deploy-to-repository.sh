cp -R "build/$BUILD_TARGET/$BUILD_FULL_NAME/"* deployment

cd deployment

git add .
git commit -m "Version $VERSION_NUMBER"
git push