# POC of Anaplan connector

An example connector for Anaplan which keeps information in memory and later pushes it to Anaplan periodically. Notice that it's a POC only, as data is not stored in a permanent storage.

## Usage

1. Pack the project using `dotnet pack /p:Version=XXX.YYY.ZZZ -o output-dir-for-package`
1. Install NuGet package to CluedIn
1. Add a new export target based on Anaplan connector
1. Add a new Stream

### ⚠⚠⚠ WARNING

At the moment the connector containers are stored in memory, therefore you have to re-process streams after CluedIn reboot, so that containers are re-created. Otherwise new stream updates will be ignored and not exported to Anaplan.

## Misc

- At the moment data is exported to Anaplan each 4 hours