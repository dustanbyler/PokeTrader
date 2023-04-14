import Vue from 'vue'
import Vuex from 'vuex'
import axios from 'axios'

Vue.use(Vuex)

/*
 * The authorization header is set for axios when you login but what happens when you come back or
 * the page is refreshed. When that happens you need to check for the token in local storage and if it
 * exists you should set the header so that it will be attached to each request
 */
const currentToken = localStorage.getItem('token')
const currentUser = JSON.parse(localStorage.getItem('user'));

if(currentToken != null) {
  axios.defaults.headers.common['Authorization'] = `Bearer ${currentToken}`;
}

import CollectionService from '../services/CollectionService';

export default new Vuex.Store({
  state: {
    token: currentToken || '',
    user: currentUser || {},
    currentCollection: '',
    currentCollectionObject: {},
    currentCollectionValue: 0,
    totalCardsInCurrentCollection: 0,
    uniqueCardsInCurrentCollection: 0,
    numberCardsForTradeInCurrentCollection: 0,
    numberCommonCards: 0,
    numberUncommonCards: 0,
    numberRareCards: 0,
    searchedCardResult: {},
    isPremium: false,
    isPublic: false
  },
  mutations: {
    SET_AUTH_TOKEN(state, token) {
      state.token = token;
      localStorage.setItem('token', token);
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
    },
    SET_USER(state, user) {
      state.user = user;
      localStorage.setItem('user',JSON.stringify(user));
    },
    LOGOUT(state) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      state.token = '';
      state.user = {};
      axios.defaults.headers.common = {};
    },
    SET_CURRENT_COLLECTION(state, username) {
      state.currentCollection = username;
    },
    SET_CURRENT_COLLECTION_OBJECT(state) {
       CollectionService.getCollectionByUser(state.currentCollection).then((response) => {
        state.currentCollectionObject = response.data;
        state.currentCollectionValue = 0;
        state.totalCardsInCurrentCollection = 0;
        state.uniqueCardsInCurrentCollection = 0;
        state.numberCardsForTradeInCurrentCollection = 0;
        state.numberCommonCards = 0,
        state.numberUncommonCards = 0,
        state.numberRareCards = 0,
        state.currentCollectionObject.forEach((collectionItem) => {
            if (collectionItem.card.price.length < 13){
              let price = parseFloat(collectionItem.card.price);
              price = price * collectionItem.quantity;
              state.currentCollectionValue += price;
              state.currentCollectionValue = parseFloat(state.currentCollectionValue.toFixed(2));
            }

          state.totalCardsInCurrentCollection += collectionItem.quantity;
          if(collectionItem.card.rarity == 'Common') {
            state.numberCommonCards += collectionItem.quantity;
          } else if(collectionItem.card.rarity == 'Uncommon') {
            state.numberUncommonCards += collectionItem.quantity;
          } else {
            state.numberRareCards += collectionItem.quantity;
          }
          state.uniqueCardsInCurrentCollection += 1;
          state.numberCardsForTradeInCurrentCollection += collectionItem.quantityForTrade;
        });
      });
    },
    ADD_TO_COLLECTION(state, collectionItem){
      var collectionArray = Object.values(this.currentCollectionObject);
      collectionArray.unshift(collectionItem);
      console.log(this.currentCollectionObject);
    },
    SET_SEARCHED_CARDS(state, result){
      state.searchedCardResult = result;
    },
    SET_PREMIUM(state, user) {
      state.isPremium = user.premiumStatus;
    },
    GO_PREMIUM(state)
    {
      state.isPremium = true;
      let premiumChanger = JSON.parse(localStorage.getItem('user'));
      premiumChanger.premiumStatus = state.isPremium;
      localStorage.setItem('user', JSON.stringify(premiumChanger));
    },
    SET_VISIBILITY(state, user) {
      state.isPublic = user.isPublic;
    },
    SWITCH_PUBLIC(state) {
      state.user.isPublic = !state.user.isPublic;
     let thing = JSON.parse(localStorage.getItem('user'));
     thing.isPublic = state.user.isPublic;
     localStorage.setItem('user', JSON.stringify(thing));
    }
  }
})
