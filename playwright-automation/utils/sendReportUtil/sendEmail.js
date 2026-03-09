require('dotenv').config();
const nodemailer = require('nodemailer');

async function sendEmail() {

    const transporter = nodemailer.createTransport({
        service: 'gmail',
        auth: {
            user: process.env.MAIL_USER,
            pass: process.env.MAIL_PASS
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