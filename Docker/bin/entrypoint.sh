#!/bin/sh
echo "Starting Fusion Dedicated Server"

while getopts s:r:l:i:p: flag
do
    case "${flag}" in
        s) session="-session ${OPTARG}";;       # custom session name
        r) region="-region ${OPTARG}";;         # custom region
        l) lobby="-lobby ${OPTARG}";;           # custom lobby
        i) publicip="-publicip ${OPTARG}";;     # custom public ip
        p) publicport="-publicport ${OPTARG}";; # custom public port
    esac
done

echo "Connecting to session: $session"

# Run Server
cd ~/bin
./GitHub_Actions_Sandbox_Server.x86_64 -batchmode -nographics $session $region $lobby $publicip $publicport -logFile

# Store server execution Exit Code
status=$?

if test $status -eq 0
then
    echo "Server exited normally"
elif test $status -eq 1
then
    echo "Server exited by timeout with no players"
else
    echo "Server exited with code: $status"
fi

echo "Done"