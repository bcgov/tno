/** used to create an array of the alphabet for the filter media narrow down options */
export const alphabetArray = () => {
  const alphabet: string[] = [];
  alphabet.push('All');
  for (let charCode = 'A'.charCodeAt(0); charCode <= 'Z'.charCodeAt(0); charCode++) {
    alphabet.push(String.fromCharCode(charCode));
  }
  return alphabet;
};
