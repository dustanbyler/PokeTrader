<template>
  <div id="register" class="text-center">
    <form @submit.prevent="register">
      <h1>Create Account</h1>
      <div role="alert" v-if="registrationErrors">
        {{ registrationErrorMsg }}
      </div>
      <div class="form-input-group">
        <label for="username">Username:</label>
        <input type="text" id="username" v-model="user.username" required autofocus />
      </div>
      <div class="form-input-group">
        <label for="password">Password:</label>
        <input type="password" id="password" v-model="user.password" required />
      </div>
      <div class="form-input-group">
        <label for="confirmPassword">Confirm Password:</label>
        <input type="password" id="confirmPassword" v-model="user.confirmPassword" required />
      </div>
      <div class="form-input-group">
        <label for="email">Email:</label>
        <input type="text" id="email" v-model="user.email" required />
      </div>
      <div class="form-input-group">
        <label for="streetAddress">Street Address:</label>
        <input type="text" id="streetAddress" v-model="user.streetAddress" required />
      </div>
      <div class="form-input-group">
        <label for="city">City:</label>
        <input type="text" id="city" v-model="user.city" required />
      </div>
      <div class="form-input-group">
        <label for="stateAbbreviation">State:</label>
        <input type="text" id="stateAbbreviation" v-model="user.stateAbbreviation" required />
      </div>
      <div class="form-input-group">
        <label for="zipCode">Zip Code:</label>
        <input type="number" id="zipCode" v-model="user.zipCode" required />
      </div>
      <button type="submit">Create Account:</button>
      <p><router-link class="login-link" :to="{ name: 'login' }">Already have an account? Log in</router-link></p>
    </form>
  </div>
</template>

<script>
import authService from '../services/AuthService';

export default {
  name: 'register',
  data() {
    return {
      user: {
        username: '',
        password: '',
        confirmPassword: '',
        role: 'user',
        email: '',
        streetAddress: '',
        city: '',
        stateAbbreviation: '',
        zipCode: 0,
        isPremium: false
      },
      registrationErrors: false,
      registrationErrorMsg: 'There were problems registering this user.',
    };
  },
  methods: {
    register() {
      if (this.user.password != this.user.confirmPassword) {
        this.registrationErrors = true;
        this.registrationErrorMsg = 'Password & Confirm Password do not match.';
      } else {
        authService
          .register(this.user)
          .then((response) => {
            if (response.status == 201) {
              this.$router.push({
                path: '/login',
                query: { registration: 'success' },
              });
            }
          })
          .catch((error) => {
            const response = error.response;
            this.registrationErrors = true;
            if (response.status === 400) {
              this.registrationErrorMsg = 'Bad Request: Validation Errors';
            }
          });
      }
    },
    clearErrors() {
      this.registrationErrors = false;
      this.registrationErrorMsg = 'There were problems registering this user.';
    },
  },
};
</script>

<style scoped>
.text-center {
  background-image: none;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: url('../assets/pokeball wallpaper.jpg');
}
form {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-radius: 100px;
  width: 500px;
  height: 500px;
  color: #3466af;
  background-color: #ffcb05;
  border-width: 20px;
  border-style: solid;
  border-color: #3466af;
  font-size: 1.25em;
  padding-bottom: 10px;
  line-height: .55em;
  text-align: top;
}
h1 {
  font-size: 2.5em;
  padding-bottom: 20px;
}
.form-input-group {
  margin-bottom: 1rem;
}
label {
  margin-right: 0.5rem;
}
.login-link {
  color: #3466af;
}
button {
  background-color: #3466af;
  color: white;
  border: none;
  border-radius: 5px;
  font-family: 'Pokemon Solid', sans-serif;
  text-align: center;
  text-justify: auto;
  letter-spacing: 1px;
  padding-bottom: 5px;
  cursor: pointer;
}
</style>
