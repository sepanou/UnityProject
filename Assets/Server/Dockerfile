FROM ubuntu

ARG DEBIAN_FRONTEND=noninteractive
ARG DOCKER_VERSION=17.06.0-ce

WORKDIR /root/

COPY build/ /root/build/

RUN chmod +x /root/build/Linux.x86_64

ENTRYPOINT ["/root/build/Linux.x86_64"]