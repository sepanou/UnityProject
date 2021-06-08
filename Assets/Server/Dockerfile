FROM ubuntu:xenial

ARG DEBIAN_FRONTEND=noninteractive
ARG DOCKER_VERSION=17.06.0-ce

RUN apt-get update && \
    apt-get install -y libglu1 xvfb libxcursor1

WORKDIR /root/

COPY Assets/Server/entrypoint.sh /entrypoint.sh

COPY build/ /root/build/

ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]