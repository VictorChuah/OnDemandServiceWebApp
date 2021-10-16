import Vue from 'vue'
import VueRouter from 'vue-router'
import App from './FirstComponent.vue'

var router = new VueRouter();

new Vue({
  el: '#app',
  router,
  components: {
    App
  },
  /*data:() => ({
    vueMessage: 'Message from Vue',
    secMessage: '2nd message'
  })*/
  data() {
    return {
      vueMessage: 'Message from Vue',
      secMessage: '2nd message ?'
    }
  }
})
