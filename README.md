# ColdFusion Dependency Mapper

## Purpose
This project is a rough-and-dirty code parser which crawls through ColdFusion
source files to build a graph of all referenced components or included files.

## Mechanics
The project prompts the user for the web root and any root component directories. It then
traverses the directory tree to build up a queue containing each code file. It then
searches each file for the following:

- `createObject` statements
- `new` statements
- `<cfinclude>` tags (CFML)
- `include` statements (cfscript)

The program then tries to locate the file by a best-effort strategy. This reference then
becomes an edge of the graph.

## Limitations

### Parsing
This is not a true lexer/parser. It uses regular expressions to locate common component
instantiation syntax. It recognizes commented-out code as code references, even though
the references are not in use.

### Locating Referenced Components
This program does not match the versatility of the native ColdFusion component locator.
It assumes the code uses the fully qualified component path. For example, in CF both of
the following are valid ways to instantiate the `com.acme.accounting.utilities` component:

```
<cfscript>
    obj = new Utilities();
    obj = new com.acme.accounting.Utilities();
</cfscript>
```

This program would only locate the latter as a valid file.

## Output File Format
The program serializes the graph to a JSON file for input to other applications. The
format consists of a dictionary where each key is a node in the graph, and the value
for each entry is an array of file names referenced by the key file.

```
{
    "C:\\Source\\webRoot\\test.cfm": [
        "C:\\Source\\webRoot\\application.cfm",
        "C:\\Source\\components\\utilities.cfc"
    ],
    ...
}
```