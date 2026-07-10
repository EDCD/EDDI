# Amazon Polly

Amazon Polly can provide cloud-hosted text-to-speech voices for EDDI. To use these voices, create or sign in to an AWS account, create a limited access key for EDDI, then configure an Amazon Polly provider profile in EDDI.

AWS generally recommends temporary credentials instead of long-term access keys. Those options are better for teams and hosted workloads, but they require extra setup outside EDDI, such as IAM Identity Center, AWS CLI profiles, or role-based credential tooling. For a first-time desktop setup that you do not expect to manage again, EDDI uses the simpler path: a dedicated IAM user access key with only the Polly permissions EDDI needs.

Do not use AWS root account access keys.

## Create or Sign In to an AWS Account

1. Open https://aws.amazon.com/ and choose **Create an AWS Account** if you do not already have one.
2. Complete the AWS account setup steps. AWS may require contact details, a payment method, and account verification.
3. Sign in to the AWS Management Console at https://console.aws.amazon.com/.

You are responsible for any AWS costs incurred by Amazon Polly usage. Review the current Polly pricing and free tier details before enabling the provider.

## Choose a Region

1. In the AWS Management Console, use the region selector in the top-right corner.
2. Choose a region near your usual location to reduce speech synthesis latency, such as `us-east-1`, `us-west-2`, `eu-west-1`, or another region where Amazon Polly is available.
3. If you want neural voices, choose a region where Amazon Polly returns neural voices for the languages you use. Some valid Polly regions may return only standard voices.
4. Note the region code. EDDI needs the region code, not the display name.

## Create Limited IAM Permissions

Grant only the permissions EDDI needs. The minimum permissions are:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "polly:DescribeVoices",
        "polly:SynthesizeSpeech"
      ],
      "Resource": "*"
    }
  ]
}
```

One setup path is:

1. In the AWS Management Console, open **IAM**.
2. Create a customer managed policy using the limited Polly JSON above.
3. Create an IAM user intended only for EDDI's Amazon Polly access.
4. Attach the limited Polly policy to that user.
5. From the IAM user, open **Security credentials** and create an access key.
6. If AWS asks for the access key use case, choose the option for an application running outside AWS. AWS may recommend an alternative sign-in method. Ignore it and hit "Next".
7. Copy the **Access key ID** and **Secret access key**. AWS only shows the secret access key once.

Keep these credentials private. Access keys are long-term credentials, so protect, monitor, rotate, and delete them when they are no longer needed. If a key is exposed, deactivate or delete it in IAM and create a replacement key.

## Configure EDDI

1. Open EDDI's **Text-to-Speech** tab.
2. Select **Manage Web Voice Providers...**.
3. Choose **Amazon Polly** in the provider picker, then select **Add Profile**.
4. Enter a profile name if you want a custom name.
5. Paste the AWS region code into **Region**.
6. Paste the IAM access key ID into **Access key ID**.
7. Paste the IAM secret access key into **Secret access key**.
8. Optionally enter locale filters such as `en`, `en-US`, or `de-DE` to filter out voices not designed to be compatible with the speech you are generating.
9. Select **Verify** to confirm EDDI can connect to Amazon Polly.
10. Select **Save**.
11. Back on the **Text-to-Speech** tab, select an Amazon Polly voice from the voice list and test it.

## Notes

- Amazon Polly voices require a working internet connection and an AWS account with Polly access.
- EDDI stores the access key values in the provider profile configuration, similar to Azure Speech Services API keys.
- Use **Clear Credentials** or remove the profile if you no longer want EDDI to retain access key values.
- Local EDDI PLS lexicon files are not uploaded to Amazon Polly. Manage Polly lexicons in AWS if you need service-side lexicon support.
