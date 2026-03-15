const  fs = require('fs');
const archiver = require('archiver');

async function zipHtmlReport() {
    return new Promise((resolve, reject) => {
        const output = fs.createWriteStream('html-report.zip');
        const archive = archiver('zip', {
            zlib: { level:9 }
        });

        output.on('close', () => {
            console.log(`HTML report zipped successfully.`);
            resolve();
        });

        archive.on('error', (err) => {
            reject(err);
        });
        archive.pipe(output);
        archive.directory('playwright-report/', false);
        archive.finalize();
    });
}

module.exports = zipHtmlReport;