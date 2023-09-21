# XML Utility CLI Tool

This tiny utility tool provides the following functions:

- check a set of XML files for **well-formedness**;
- check a set of XML files for **validity** against a given schema;
- **transform** a set of XML files using a given XSLT 1.0 stylesheet.

## Commands

Open a command prompt in the directory where the `xutil` executable is located and type `./xutil` to get the list of available commands.

### Check XML Well Formedness

Syntax:

```bash
./xutil check <InputFileMask> [-r]
```

- `InputFileMask`: input file(s) mask (you can use `?` or `*` wildcards).
- `-r`: recursive search (search in subdirectories).

Example:

```bash
./xutil check c:/users/dfusi/desktop/xml/*.xml
```

### Validate XML

Syntax:

```bash
./xutil validate <SchemaFilePath> <InputFileMask> [-r] [-d]
```

- `InputFileMask`: input file(s) mask (you can use `?` or `*` wildcards).
- `-r`: recursive search (search in subdirectories).
- `-d`: display distinct error messages only.

Example:

```bash
./xutil validate c:/users/dfusi/desktop/xml/schema.xsd c:/users/dfusi/desktop/xml/*.xml
```

### Transform XML

Syntax:

```bash
./xutil transform <XsltFilePath> <InputFileMask> <OutputDir> [-x <EXTENSION>]
```

- `XsltFilePath`: the XSLT 1.0 stylesheet file path.
- `InputFileMask`: input file(s) mask (you can use `?` or `*` wildcards).
- `OutputDir`: output directory.
- `-x`: the output file(s) extension if it should be changed.

Example:

```bash
./xutil transform c:/users/dfusi/desktop/xml/html.xslt c:/users/dfusi/desktop/xml/*.xml c:/users/dfusi/desktop/xml/html -x .html
```
