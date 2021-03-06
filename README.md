# ColdFusion Dependency Mapper

## Purpose
This project is a rough-and-dirty code parser which crawls through ColdFusion
source files to build a graph of all referenced components or included files.

<div style="text-align: center">
    <a href="https://i.imgur.com/uaPoKIf.jpg" target="_blank">
        <img src="https://i.imgur.com/uaPoKIf.jpg" width="400px" alt="Example Network Diagram">
    </a>
</div>

## Mechanics
The project prompts the user for the web root and any root component directories. It then
traverses the directory tree to build up a queue containing each code file. It then
searches each file for the following:

- `createObject` statements
- `new` statements
- `<cfinclude>` tags (CFML)
- `include` statements (cfscript)

The program then tries to locate any references by a best-effort strategy. These references then
become edges of the graph.

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

## Visualizer Script
The visualize python script uses [Networkx](https://networkx.github.io/) and [Matplotlib](https://matplotlib.org/) to arrange and print the nodes. Use
```
py visualize.py -h
```
for a list of options.

## Usage
1. Build and execute the C# application using Visual Studio.
2. Provide the application with the full directory path to the Web Root directory for the ColdFusion application. Provide any additional component directories.
3. Give a file path for the output JSON file
4. Execute visualize.py
```
py visualize.py <file.json> <graph title>
```
