import Vue from 'vue'

const router = new VueRouter({
  routes: [
    { path: 'page1', component: load('page1') }
    { path: 'page2', component: load('page2') }
  ]
});

new Vue({
  el: '#app',
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
