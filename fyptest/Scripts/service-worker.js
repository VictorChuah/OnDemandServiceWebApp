if ('serviceWorker' in navigator) {
  navigator.serviceWorker.getRegistrations().then(function (registrations) { for (let registration of registrations) { registration.unregister() } })

  navigator.serviceWorker.register('Scripts/service-worker.js').then(function (registration) {
    // Registration was successful 
    console.log('ServiceWorker registration successful with scope: ', registration.scope);
    registration.pushManager.subscribe({ userVisibleOnly: true }).then(function (subscription) {
      isPushEnabled = true;
      console.log("subscription.subscriptionId: ", subscription.subscriptionId);
      console.log("subscription.endpoint: ", subscription.endpoint);

      // TODO: Send the subscription subscription.endpoint
      // to your server and save it to send a push message
      // at a later date
      return sendSubscriptionToServer(subscription);
    });
  }).catch(function (err) {
    // registration failed :(
    console.log('ServiceWorker registration failed: ', err);
  });
}

//AIzaSyCDoA2c2_hCf6pNUMV_T5u3WTR0hwV9Vaw

//339259033751
