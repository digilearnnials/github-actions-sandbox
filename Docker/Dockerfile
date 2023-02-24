# base image
FROM ubuntu:bionic

# install
RUN apt-get --yes update && \
    apt-get --yes clean && \
    apt-get --yes install sudo && \
    apt-get --yes install ca-certificates

# add and setup fusion user
RUN useradd -m fusion && \
    echo "fusion:fusion" | chpasswd && \
    adduser fusion sudo

# copy server binary files
COPY --chown=fusion:fusion bin /home/fusion/bin

# set permissions on entrypoint
RUN chmod +x /home/fusion/bin/entrypoint.sh

# execute container as this user
USER fusion

# setup start script
ENTRYPOINT ["/home/fusion/bin/entrypoint.sh"]

# Fusion Server Port
EXPOSE 27015/udp