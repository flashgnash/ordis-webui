#!/usr/bin/env bash

dotnet blazorfmt format | awk '
BEGIN { filename = ""; collecting = 0 }
/^=== / {
  path = $2
  if (path ~ /\.direnv/) {
    collecting = 0
    next
  }
  if (filename != "") {
    close(outfile)
  }
  filename = path
  outfile = filename
  collecting = 1
  next
}
collecting {
  print $0 >> outfile
}
'
