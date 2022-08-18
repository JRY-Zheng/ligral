CURRENT_DIR="$( cd "$( dirname $0 )"; pwd )"
TEMP_DIR=/tmp/ligral
cd /tmp
if [ -d "ligral" ];then
    rm -r ligral
fi
mkdir ligral
cd ligral

cp -r $CURRENT_DIR/ligral-dpkg/ $TEMP_DIR/ligral-dpkg/
INSTALLER=ligral-v0.2.3-linux-x64.deb
pwd
ls -l
sudo dpkg -b ligral-dpkg $INSTALLER
cp $TEMP_DIR/$INSTALLER $CURRENT_DIR/$INSTALLER
