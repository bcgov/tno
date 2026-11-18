const nodemailer = require('nodemailer');
const { loadEnv, requireEnv } = require('../env');

async function sendEmail() {
    loadEnv();
    requireEnv(['MAIL_USER', 'MAIL_PASSWORD', 'MAIL_TO']);

    const transporter = nodemailer.createTransport({
        service: 'gmail',
        auth: {
            user: process.env.MAIL_USER,
            pass: process.env.MAIL_PASSWORD
        }
    });

    const mailOptions = {
        from: process.env.MAIL_USER,
        to: process.env.MAIL_TO,
        subject: 'Automation Test Report - HTML',
        text: 'Please find the attched HTML execution report.',
        atrachments: [
            {
                filename: 'html-report.zip',
                path: './html-report.zip'
            }
        ]
    };

    await transporter.sendMail(mailOptions);

    console.log('Email sent successfully with Allure report.');
}

module.exports = sendEmail;
