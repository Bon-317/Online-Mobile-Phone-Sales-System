document.getElementById('send-otp-btn').addEventListener('click', async () => {
    const email = document.querySelector('input[name="email"]').value;
    const otp = Math.floor(100000 + Math.random() * 900000).toString();

    const params = {
        to_email: email,
        to_name: "Người dùng",
        passcode: otp,
        email: email
    };

    await emailjs.send("service_69q37oa", "template_y3f2spm", params, "kezSu7ZgYe7uy12BJ");

    fetch('/Session/SetOtp', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ otp })
    });

    document.getElementById('otp-section').style.display = 'block';
});
