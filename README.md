zsm: ZFS Snapshot Manager
===

zsm is a small application that provides extremely flexible scheduling of ZFS snapshots. It does not rely on the names of snapshots to determine which snaps to remove.

It requires a recent version of Mono to be installed, and runs as a daemon (run in a screen session for best results).

Upon first run, zsm creates a configuration file in the current directory called 'zsm.json', this is a JSON formatted configuration file. Schedules and other options can be defined within. It also will create a 'history.json' file, this is used to store information about snapshots that have been taken by zsm and it loads this file when restarted to avoid lengthy querying of zfs metadata.
