const zipHtmlReport = require('./zipReport');
const sendEmail = require('./sendEmail');

async function generateAndSendHtmlReport() {

    try {
        console.log('Zipping HTML report...');
        await zipHtmlReport();

        console.log('Sending report via email...');
        await sendEmail();

        console.log('HTML report email sent successfully!!')
    } catch (error) {
        console.error('Error while sending report: ', error);
    }
    
}

generateAndSendHtmlReport();