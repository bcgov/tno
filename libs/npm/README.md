# NPM Packages

You will need an NPM account to publish packages [npmjs.com](https://npmjs.com).

To publish a package you will need to login.

```bash
npm login
# Enter your username
# Enter your password
# Enter your email address
# Enter your 2-Factor Authentication Code
```

Publish the package, and enter a version.

```bash
yarn publish
# Enter a new version number
```

## Testing Package Locally with Publishing

Testing a package before publishing is critical.
To do this use the following commands to install a package locally.

More information [here](https://dev.to/vcarl/testing-npm-packages-before-publishing-h7o)

```bash
# Create a package
yarn pack
# Move the package to your home directory
mv package-name-0.0.0.tgz ~
# Install the package in your project
yarn add ~/package-name-0.0.0.tgz
```
