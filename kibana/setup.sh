#!/bin/sh

cd /configure
ansible-playbook configure-kibana.yml &

/usr/local/bin/kibana-docker
