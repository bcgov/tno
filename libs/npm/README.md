# NPM Packages

## Testing Package Locally prior to Publishing

Testing a package before publishing is critical.
To do this use the following commands to install a package locally.

More information [here](https://dev.to/vcarl/testing-npm-packages-before-publishing-h7o)

```bash
# From the libs/npm/core folder, create a package for either the editor or subscriber app
make pack n=<editor|subscriber>
# Now from the root folder of the repo refresh the app you chose in the prior command
make refresh n=<editor|subscriber>
```
## NPM Publishing

You will need an NPM account to publish packages [npmjs.com](https://npmjs.com).

To publish a package you will need to login.

```bash
npm login
# Enter your username
# Enter your password
# Enter your email address
# Enter your 2-Factor Authentication Code
```

*Once you have an NPM account, you need to be request access to be able to publish the tno-core library.*

Publish the package, and enter a version.

```bash
make publish
# Enter a new version number
```

