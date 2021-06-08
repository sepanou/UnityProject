FROM ubuntu:bionic

WORKDIR /root/

COPY server-build/ .

RUN chmod +x /root/Linux.x86_64

ENTRYPOINT ["/root/Linux.x86_64"]