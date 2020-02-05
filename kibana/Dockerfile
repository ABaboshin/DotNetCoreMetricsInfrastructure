FROM kibana:7.5.2

USER root

RUN yum install -y epel-release
RUN yum install -y ansible dos2unix

COPY . /configure

RUN dos2unix /configure/setup.sh

USER kibana

ENTRYPOINT [ "/configure/setup.sh" ]
