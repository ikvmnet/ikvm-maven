# IKVM.Maven.Sdk - IKVM support for Maven dependencies

`IKVM.Maven.Sdk` is a set of MSBuild extensions for referencing Maven artifacts within .NET SDK projects.

To use, install the `IKVM.Maven.Sdk` package from NuGet, and add a `<MavenReference />` element to your SDK-style project. Various Maven options are supported through item-metadata.

The simplest use case is to specify the group ID and artifact ID coordinates on the item specification, and use `Version` for the metadata.

```xml
<ItemGroup>
    <MavenReference Include="org.foo.bar:foo-lib" Version="1.2.3" />
</ItemGroup>
```

Optionally, use an arbitrary value for the item specification, and explicitely specify information through metadata:

```xml
<ItemGroup>
    <MavenReference Include="foo-lib">
        <GroupId>org.foo.bar</GroupId>
        <ArtifactId>foo-lib</ArtifactId>
        <Classifier></Classifier>
        <Version>1.2.3</Version>
        <Scope></Scope>
        <Exclusions></Exclusions>
    </MavenReference>
</ItemGroup>
```

## Underspecified Dependencies

It is fairly common for Java developers to underspecify dependencies within Maven. For instance, if their library is often used
as a dependency of another aggregate package, or another library, at runtime, they can be reasonably certain classes they rely on will exist.
Also, if they do not exist, but the specific code path that requires dependent classes is never hit, users won't experience any issue.

However, since IKVM is statically compiling assemblies, we need to be able to properly track dependencies between
each assembly we might be building. We can't fully build Library A if it depends on missing classes from Library B.
As such, IKVM.Maven.Sdk requires that Maven dependencies be fully specified.

But, we aren't the authors of Maven artifacts. Nor can we provide local overrides for missing Maven dependencies, as the
Maven artifacts need to be available to any user who might indirectly add a reference through a NuGet package. Therefor,
if you encounter an underspecified or missing dependency in Maven, the proper place to fix it is in Maven. Report the missing
dependency to the authors of the Maven library you are attempting to use.

## Transitive Dependencies

The `<MavenReference />` item group operates similar to a `dependency` in Maven. All transitive dependencies are
collected and resolved, and then the final output is produced. However, unlike `PackageReference`s, `MavenReference`s
are collected by the final output project, and reassessed. That is, each dependent Project within your .NET
SDK-style solution contributes its `MavenReference`s to project(s) which include it, and each project makes its own
dependency graph. Projects do not contribute their final built assemblies up. They only contribute their dependencies.
Allowing each project in a complicated solution to make its own local conflict resolution attempt.

`PackageReference`s are supported in the same way. Projects which include `MavenReference`s do not pack the generated IKVM
assemblies into their NuGet packages. Instead, they pack a partial POM file which only declares their dependencies. At 
build-time on the consumer's machine the final dependency graph is collected and generation happens. No generated
assemblies are published to NuGet. This prevents possible conflicts between NuGet packages and incompatible base Java
dependencies. For instance, if a package on nuget.org contained an actual copy of `commons-logging.dll`, there would be
no guarentee that this assembly was generated with the correct options to support a different package on nuget.org that
also depended on commons-logging. Since the final build machine is responsible for gathering and generating the
dependencies, these conflicts become simple Maven conflicts: multiple packages dependending on differnet versions of
the same thing, and Maven being unable to come up with a solution. Basically, not our problem.

`MavenReference`s fully support TFMs. A `<MavenReference />` element can be conditional based on TFM. As the partial
packaged POM-file is indexed by TFM in the generated .nupkg.

## Assembly Generation

Assembly generation options are limited. Users are not allowed to customize the assembly name, version, or other
optimization information that IKVM's compiler uses to produce the output. This is to ensure that NuGet packages that
depend on generated assemblies do so under a certain set of assumptions that can continue to be met. As non-Java
assemblies published in NuGet packages are compiled against on certain assembly names and version, allowing different
people to rename or change assemblies away from their default would break the expectation that two NuGet packages that
depend on the same Maven artifact resolve to the same assembly name.
