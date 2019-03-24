## Contributing

Welcome and thank you for your interest in contributing to these common libraries!


### Compiling Code

Project uses .NET 4.0, .NET 4.7, .NET Standard 1.6, and .NET Standard 2.0.
Visual Studio 2017 is the preferred development environment.

All code is situated in ```Medo``` directory structure. Each project type then
gets a file link toward it to avoid file duplication.


### Contributing Fixes

Fixes can be contributed using pull requests. Do note project is under MIT
license and any contribution will inherit the same.

This repository uses K&R coding style. Please do make sure any contribution
follows coding style already in use. If in doubt, just rely on autoformatting -
both VS Code OmniSharp and Visual Studio 2017 editor config are supported.

LF line ending is strongly preffered. To have Git check it for you, configure
`core.whitespace` setting:

    git config core.whitespace blank-at-eol,blank-at-eof,space-before-tab,cr-at-eol

All textual files should be encoded as UTF-8 without BOM.


### Coding style

For non code files (xml, etc), use the best judgment. Albeit, do note that `tab`
character should be avoided and a number of `space` characters should be used
instead (4 being preffered).

For source code files, the general rule we follow is K&R with a sparkling of
Visual Studio defaults while using [.NET corefx style](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md)
for corner cases.

1.  Use [K&R](https://en.wikipedia.org/wiki/Indentation_style#K&R_style) braces,
    where each brace is placed on the same line as control statement. A single
    line "if" statement block is allowed but it has to be enclosed in braces.

2.  Use four spaces of indentation (no tabs).

3.  Use `_camelCase` for internal and private fields and use `readonly` where
    possible. Prefix internal and private instance fields with `_`, static
    fields with `s_` and thread static fields with `t_`. When used on static
    fields, `readonly` should come after `static` (e.g. `static readonly` not
    `readonly static`). Public fields should be used sparingly and should use
    PascalCasing with no prefix when used.

4.  Avoid `this.` unless absolutely necessary.

5.  Always specify the visibility, even if it's the default (e.g.
    `private string _foo` not `string _foo`). Visibility should be the first
    modifier (e.g. `public abstract` not `abstract public`).

6.  Namespace imports should be specified at the top of the file, *outside* of
    `namespace` declarations, and should be sorted alphabetically, with the
    exception of `System.*` namespaces, which are to be placed on top of all
    others.

7.  Avoid more than one empty line at any time. For example, do not have two
    blank lines between members of a type.

8.  Avoid spurious free spaces. For example avoid `if (someVar == 0)...`, where
    the dots mark the spurious free spaces. Consider enabling "View White Space
    (`Ctrl+E`, `S`)" if using Visual Studio to aid detection.

9.  If a file happens to differ in style from these guidelines (e.g. private
    members are named `m_member` rather than `_member`), the existing style in
    that file takes precedence.

10. Use of `var` is strongly preferred.

11. Use language keywords instead of BCL types (e.g. `int, string, float`
    instead of `Int32, String, Single`, etc) for both type references as well as
    method calls (e.g. `int.Parse` instead of `Int32.Parse`).

12. Use PascalCasing to name all our constant local variables and fields. The
    only exception is for interop code where the constant value should exactly
    match the name and value of the code you are calling via interop.

13. Use ```nameof(...)``` instead of ```"..."``` whenever possible and relevant.

14. Fields should be specified at the top within type declarations.

15. Non-ASCII characters should use literal characters but unicode escape
    sequences (`\uXXXX`) are acceptable.

16. When using labels (for goto), label is aligned to the leftmost position.

There is both `.editorconfig` for Visual Studio 2017 and above and
`omnisharp.json` for Visual Studio Code enabling auto-formatting conforming to
the above guidelines.

As great portion of code comes before these rules were in effect there is a lot
of discrepancies. These will be sorted out in time as code gets updated.


# Thank You!
