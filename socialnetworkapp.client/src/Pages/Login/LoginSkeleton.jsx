import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import './Login.css'
import { Link, useNavigate } from 'react-router-dom';
import { userController } from '../../Constants';
import { useState } from 'react';
import { loginUser } from './Signup.jsx';



function loginFailed(setLoginState)
{
    setLoginState((oldValue) =>
    {
        setTimeout(setLoginState(oldValue),3000)
        return !oldValue
    })
}
async function login(username, password,navigate,setLoginState)
{
    try {
        const response = await fetch(`${userController}/LogIn/${username}/${password}`,{
    })
        if (response.statusText == "OK")
        {
            const jsonResponse = await response.json() 
            const user = jsonResponse.item1
            user.sessionToken = jsonResponse.item2
            loginUser(user,navigate)
            
        }
        else
        {
            loginFailed(setLoginState)
        }
    }
    catch (error)
    {
        console.log(error.message)
    }
   

}



export default function LoginSkeleton({ flavor }) { 

    const [loginFailedState,setLoginFailedState]=useState(false)
    const navigate = useNavigate()
    const [loginState,setLoginState] = useState({
        username:"",password:""
    })

    function onChange(event)
    {
        setLoginState((oldValue) =>
        {
            return {
                ...oldValue,
                [event.target.name]:event.target.value
            }
        })
    }

    return <div className="login_container">
        <h2>{flavor}</h2>
        <TextField id="Username" label="Username" variant="outlined" sx={{
            marginTop:"10px",
            marginBottom: "10px"
            
        }}
            value={loginState.username}
            name='username'
        onChange={(onChange)}/>
        <TextField id="Password" label="Password" variant="outlined" type='password'
            sx={{
                marginBottom:"10px"
            }}
            value={loginState.password}
            name='password'
        onChange={(onChange)}/>
        {loginFailedState && <p style={{
            color: 'red',
        }}>{flavor + " Failed"}</p>}
        <Button variant="text" sx={{
            width:"80px"
        }} id='Button' onClick={(ev) =>login(loginState.username, loginState.password, navigate, setLoginFailedState)
        }>OK</Button>
        <Link to={flavor===SIGN_UP?'/login':'/signup'}>{flavor==SIGN_UP?'Already have an account? Log in.':'Don\'t have an account? Sign up here.'}</Link>
       
    </div>
}

export const SIGN_UP = "Sign Up"
export const LOG_IN = "Log In"
