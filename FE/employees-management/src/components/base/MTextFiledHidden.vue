<template>
  <div>
    <label class="m-textfield-label" for="id" :title="title"
      >{{ label }}
      <span v-if="isRequired && label" class="input-required">*</span></label
    >
    <div
      :class="{
        'm-textfield': true,
        'm-texfield-err': errMsg !== '',
        'm-textfield-focus': errMsg === '' && isInputFocused,
        'm-textfield-max-input': maxInput,
        'm-textfield-input': !maxInput,
      }"
    >
      <input
        ref="inputTagRef"
        :type="type || 'text'"
        v-model="inputValue"
        :title="errMsg"
        :placeholder="placeholder"
        @input="validateInput(inputValue)"
        @focus="isInputFocused = true"
        @blur="isInputFocused = false"
      />
      <div
        class="togle-show-text"
        :class="{
          'btn-not-show': !isShowText,
          'btn-show': isShowText,
        }"
        @click="togleShowtext()"
      ></div>
    </div>
  </div>
</template>

<script>
export default {
  name: "MInputHidden",
  props: {
    label: {
      type: String,
    },
    isRequired: {
      type: Boolean,
    },
    regex: {
      type: RegExp,
    },
    validateMsg: {
      type: String,
    },
    requiredMsg: {
      type: String,
    },
    placeholder: {
      type: String,
    },
    maxInput: {
      type: Boolean,
    },
    title: {
      type: String,
    },
  },
  data() {
    return {
      inputValue: "",
      errMsg: "",
      isInputFocused: false,
      type: "password",
      isShowText: false,
    };
  },
  methods: {
    /**
     *  togle show text or not
     * 	created by: Nguyễn Thiện Thắng
     *  created_at: 2024/3/12
     */
    togleShowtext() {
      this.isShowText = !this.isShowText;
      this.type = this.type === "password" ? "text" : "password";
    },
    /**
     *  validate value if valid -> update value for data in parent component
     * @param {*} newValue - value to check is valid or not
     * @return {*} err msg
     * 	created by: Nguyễn Thiện Thắng
     *  created_at: 2024/1/20
     */
    validateInput(newValue) {
      if (this.isRequired) {
        if (newValue) {
          this.$emit("update:value", this.inputValue);
          // update value from parent component
          this.inputValue = newValue;
          this.errMsg = "";
          return this.errMsg;
        }
      }
      // regex exist
      if (this.regex) {
        if (this.regex.test(newValue)) {
          this.$emit("update:value", this.inputValue);
          this.errMsg = "";
          this.inputValue = newValue;
          return this.errMsg;
        }
      }
      // catched all valid case above when regex | required -> if this code block -> auto not valid
      if (this.regex || this.isRequired) {
        if (
          !newValue ||
          isNaN(newValue) ||
          newValue === "" ||
          newValue === null
        ) {
          this.$emit("update:value", "");
          this.errMsg = this.requiredMsg;
          this.focusInput();
          return this.errMsg;
        }
      }
      // there no required and regex
      this.inputValue = newValue;
      this.$emit("update:value", newValue);
      this.errMsg = "";
      return this.errMsg;
    },
    /**
     *  set data base parent component and display into input filed
     * @param {*} value - value from parent component
     * 	created by: Nguyễn Thiện Thắng
     *  created_at: 2024/1/20
     */
    setDataFromParent(value) {
      this.inputValue = value;
    },
    /**
     * focus into input tag
     * 	created by: Nguyễn Thiện Thắng
     *  created_at: 2024/1/20
     */
    focusInput() {
      this.$nextTick(() => {
        this.$refs.inputTagRef.focus();
      });
    },
  },
};
</script>

<style scoped>
.m-textfield-label {
  font-weight: 600;
}
.m-textfield label {
  font-weight: 700;
  font-size: 14px;
}

.input-required {
  color: red;
}

.m-textfield {
  display: flex;
  align-items: center;
}

.togle-show-text {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  margin: 0 8px;

  width: 20px;
  height: 12px;
}

.btn-show {
  background: url("../assets/img/Sprites.64af8f61.svg") no-repeat -931px -94px;
}

.btn-not-show {
  background: url("../assets/img/Sprites.64af8f61.svg") no-repeat -987px -92px;
  height: 16px;
}

/* ---------------------------input filed------------ */
.m-textfield-input {
  display: flex;
  align-items: center;
  height: 34px;
  margin-top: 8px;
  width: 100%;
  border-radius: 4px;
  border: 1px solid #e6e6e6;
}

.m-textfield-input input {
  height: 100%;
  width: 100%;
  padding: 0 12px;
  border: none;
  outline: none;
  font-size: 14px;
  border-radius: 4px;
}

.m-textfield-input input:focus {
  outline: none;
}

.m-textfield-input input::placeholder {
  color: #999999;
  font-size: 14px;
}

/* max input ( 48px )  */

.m-textfield-max-input {
  display: flex;
  align-items: center;
  height: 48px;
  margin-top: 8px;
  width: 100%;
  border-radius: 4px;
  border: 1px solid #e6e6e6;
}

.m-textfield-max-input input {
  height: 100%;
  width: 100%;
  padding: 0 12px;
  border: none;
  outline: none;
  font-size: 14px;
  border-radius: 4px;
}

.m-textfield-max-input input:focus {
  outline: none;
}

.m-textfield-max-input input::placeholder {
  color: #999999;
  font-size: 14px;
}
/* end max input */

.textfield-err {
  margin-top: 1px;
  color: red;
  height: 15px;
  background-color: white;
  font-size: 11px;
}

/* For type = date */

input::-webkit-datetime-edit,
input::-webkit-calendar-picker-indicator {
  color: #1f1f1f;
  font-family: hihi;
}
/*  end date type */

.m-texfield-err {
  border: 1px solid rgb(255, 49, 49);
}

.m-textfield-focus {
  border: 1px solid #50b83c;
}
</style>
