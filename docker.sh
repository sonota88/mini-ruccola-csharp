#!/bin/bash

set -o nounset

readonly IMAGE=mini-ruccola-csharp:2

cmd_build() {
  docker build \
    --build-arg USER=$USER \
    --build-arg GROUP=$(id -gn) \
    -t $IMAGE .
}

cmd_run() {
  docker run --rm -it \
    -v "$(pwd):/home/${USER}/work" \
    $IMAGE "$@"
}

cmd_run_i() {
  docker run --rm -i \
    -v "$(pwd):/home/${USER}/work" \
    $IMAGE "$@"
}

cmd="$1"; shift
case $cmd in
  build | b* )
    cmd_build "$@"
;; run-i )
     cmd_run_i "$@"
;; run | r* )
     cmd_run "$@"
;; * )
     echo "invalid command (${cmd})" >&2
     ;;
esac
